import os
import json
import numpy as np
from scipy.spatial.distance import euclidean
from app.face.embedding import extract_embedding
from app.models import User
from sqlalchemy.orm import Session

EMBEDDINGS_DIR = os.path.join(os.path.dirname(__file__), "embeddings")
THRESHOLD = 0.6

def load_all_embeddings():
    embeddings = {}
    for file in os.listdir(EMBEDDINGS_DIR):
        if file.endswith(".json"):
            path = os.path.join(EMBEDDINGS_DIR, file)
            user_id = file.replace(".json", "")
            with open(path, "r") as f:
                arr = json.load(f)
                embeddings[user_id] = np.array(arr)
    return embeddings

def verify_face(image_path: str):
    input_embedding = extract_embedding(image_path)
    embeddings = load_all_embeddings()

    for user_id, saved_embedding in embeddings.items():
        distance = euclidean(input_embedding, saved_embedding)
        print(f"[DEBUG] Comparing with {user_id}, Distance: {distance}")
        if distance < THRESHOLD:
            print(f"[✅] Match Found: {user_id}")
            return user_id
    return None

def verify_face_from_rds(image_path: str, db: Session):
    input_embedding = extract_embedding(image_path)

    # Query all users with face images
    users = db.query(User).filter(User.face_image != None).all()

    for user in users:
        if not user.face_image:
            continue

        # Save image temporarily
        temp_user_img = f"temp_stored_{user.username}.jpg"
        with open(temp_user_img, "wb") as f:
            f.write(user.face_image)

        try:
            stored_embedding = extract_embedding(temp_user_img)
            distance = euclidean(input_embedding, stored_embedding)
            print(f"[DEBUG] Comparing with {user.username}, Distance: {distance}")

            if distance < THRESHOLD:
                print(f"[✅] Match Found: {user.username}")
                return user.username
        except Exception as e:
            print(f"[ERROR] Failed to process {user.username}: {e}")
        finally:
            if os.path.exists(temp_user_img):
                os.remove(temp_user_img)

    return None