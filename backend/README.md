# 🧠 FastAPI Backend — HospitalAuthSystem

This is the backend API service for the Hospital Authentication System. It handles user registration, facial recognition, and credential verification.

---

## 🔧 Setup Instructions

### 📦 Requirements

- Python 3.9+
- PostgreSQL (RDS or local)
- Git LFS (for model files)

### 🔍 Installation

```bash
# Create a virtual environment
python -m venv venv
source venv/bin/activate  # or venv\Scripts\activate on Windows

# Install dependencies
pip install -r requirements.txt

Environment
Update app/database.py with your PostgreSQL URL:
DATABASE_URL = "postgresql://<user>:<password>@<host>:<port>/<db>"

Run the Backend
bash
Copy
Edit
uvicorn app.main:app --reload
Then open: http://127.0.0.1:8000/docs

📁 Endpoints
Route	Method	Description
/register/	POST	Register user with credentials + face
/verify/	POST	Verify face against stored embeddings
/login/	POST	Login with username + password
/delete_user/	POST	Delete a user

🤖 Model Files
Tracked via Git LFS:

shape_predictor_68_face_landmarks.dat

📌 Notes
Face embeddings are stored as BYTEA in PostgreSQL.

Uses dlib, OpenCV, scipy for face recognition.
