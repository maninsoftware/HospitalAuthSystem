import dlib
import cv2
import numpy as np
import os
import json

base_path = os.path.dirname(__file__)
predictor_path = os.path.join(base_path, "models", "shape_predictor_68_face_landmarks.dat")
face_model_path = os.path.join(base_path, "models", "dlib_face_recognition_resnet_model_v1.dat")
EMBEDDING_DIR = os.path.join(base_path, "embeddings")

if not os.path.exists(EMBEDDING_DIR):
    os.makedirs(EMBEDDING_DIR)

detector = dlib.get_frontal_face_detector()
predictor = dlib.shape_predictor(predictor_path)
face_model = dlib.face_recognition_model_v1(face_model_path)

def extract_embedding(image_path: str):
    img = cv2.imread(image_path)
    if img is None:
        raise ValueError("Invalid image")
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    faces = detector(img_rgb)
    if len(faces) == 0:
        raise ValueError("No face detected")
    shape = predictor(img_rgb, faces[0])
    embedding = face_model.compute_face_descriptor(img_rgb, shape)
    return np.array(embedding)

def save_embedding(user_id: str, embedding: np.ndarray):
    path = os.path.join(EMBEDDING_DIR, f"{user_id}.json")
    with open(path, "w") as f:
        json.dump(embedding.tolist(), f)
