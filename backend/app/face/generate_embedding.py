import dlib
import cv2
import numpy as np

# Load models
detector = dlib.get_frontal_face_detector()
import os

base_dir = os.path.dirname(__file__)
predictor_path = os.path.join(base_dir, 'models', 'shape_predictor_68_face_landmarks.dat')
face_rec_model_path = os.path.join(base_dir, 'models', 'dlib_face_recognition_resnet_model_v1.dat')

predictor = dlib.shape_predictor(predictor_path)
face_rec_model = dlib.face_recognition_model_v1(face_rec_model_path)


def get_face_embedding(image_path):
    img = cv2.imread(image_path)
    if img is None:
        raise ValueError("Image not found or unreadable")

    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    faces = detector(img_rgb)

    if len(faces) == 0:
        raise ValueError("No face detected")
    
    shape = predictor(img_rgb, faces[0])
    face_descriptor = face_rec_model.compute_face_descriptor(img_rgb, shape)

    return np.array(face_descriptor)

if __name__ == "__main__":
    embedding = get_face_embedding(r"D:\Codelulu Solutions\Projects\POC-Windows Authentication Application\Project Files\auth-backend\app\face\sample_images\person1.jpg")
    print("128D embedding:\n", embedding)
