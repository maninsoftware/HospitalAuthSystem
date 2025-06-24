from sqlalchemy.orm import Session
from app.models import User

def get_user(db: Session, username: str):
    return db.query(User).filter(User.username == username).first()

def create_user(db: Session, username: str, password: str, embedding: list = None):
    user = User(username=username, password=password, face_embedding=embedding)
    db.add(user)
    db.commit()
    db.refresh(user)
    return user

def delete_user(db: Session, username: str):
    user = db.query(User).filter(User.username == username).first()
    if user:
        db.delete(user)
        db.commit()