from sqlalchemy import Column, String, Integer, JSON
from app.database import Base

class User(Base):
    __tablename__ = "users"
    username = Column(String, primary_key=True, index=True)
    password = Column(String)
    face_embedding = Column(JSON, nullable=True)
