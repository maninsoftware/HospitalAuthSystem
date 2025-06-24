import os
import numpy as np
from fastapi import FastAPI, UploadFile, File, Form, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.orm import Session
from scipy.spatial.distance import euclidean

from app.database import get_db
from app.models import Base, User
from app.database import engine

from app.crud import create_user, get_user, delete_user
from app.face.embedding import extract_embedding, save_embedding
from app.face.verify_face import verify_face, verify_face_from_rds
#Base.metadata.drop_all(bind=engine)
Base.metadata.create_all(bind=engine)

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/register/")
async def register_user(
    username: str = Form(...),
    password: str = Form(...),
    file: UploadFile = File(...),
    db: Session = Depends(get_db),
):
    if get_user(db, username):
        raise HTTPException(status_code=400, detail="User already exists")

    image_bytes = await file.read()
    temp_path = f"temp_{username}.jpg"
    with open(temp_path, "wb") as buffer:
        buffer.write(image_bytes)

    try:
        embedding = extract_embedding(temp_path).tolist()  # Convert numpy -> list
        user = create_user(db, username, password, embedding=embedding)
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Face registration failed: {str(e)}")
    finally:
        os.remove(temp_path)

    return {"message": "User registered successfully", "username": username}

@app.post("/verify/")
async def verify_uploaded_face(file: UploadFile = File(...), db: Session = Depends(get_db)):
    temp_path = "temp_uploaded.jpg"
    with open(temp_path, "wb") as buffer:
        buffer.write(await file.read())

    try:
        input_embedding = extract_embedding(temp_path)

        users = db.query(User).filter(User.face_embedding != None).all()

        for user in users:
            saved_embedding = np.array(user.face_embedding)
            dist = euclidean(input_embedding, saved_embedding)

            if dist < 0.6:  # tune threshold
                return {"status": "success", "username": user.username}

        raise HTTPException(status_code=404, detail="Face not recognized")
    finally:
        os.remove(temp_path)

@app.post("/login/")
async def login_user(username: str = Form(...), password: str = Form(...), db: Session = Depends(get_db)):
    user = get_user(db, username)
    if user and user.password == password:
        return {"message": f"Welcome {username}!"}
    raise HTTPException(status_code=401, detail="Invalid credentials")

EMBEDDINGS_DIR = "app/face/embeddings"  # adjust if needed

@app.post("/delete_user/")
def delete_user_route(username: str = Form(...), db: Session = Depends(get_db)):
    user = get_user(db, username)
    if not user:
        raise HTTPException(status_code=404, detail="User not found")

    # Delete from DB
    delete_user(db, username)

    # Delete embedding file
    embedding_path = os.path.join(EMBEDDINGS_DIR, f"{username}.json")
    if os.path.exists(embedding_path):
        os.remove(embedding_path)

    return {"message": f"User '{username}' deleted successfully"}