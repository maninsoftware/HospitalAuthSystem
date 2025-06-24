# 🏥 Hospital Authentication System

A secure Windows-based login shell for hospital PCs using FastAPI (backend) and WPF (frontend). Supports both username/password and facial recognition login with offline caching and kiosk-mode support.

---

## 🔧 Project Structure

HospitalAuthSystem/
│
├── backend/ # FastAPI backend with face recognition and RDS integration
├── frontend/ # WPF C# login shell for Windows
├── .gitattributes # Git LFS config for large files
└── README.md # Main project readme


---

## 🚀 Features

- ✅ Username + password authentication (stored in AWS RDS)
- ✅ Face login via webcam (embedding stored in RDS)
- ✅ Kiosk mode for Windows (shell replacement)
- ✅ Offline login support with local cache
- ✅ Git LFS for large model files

---

## 💻 Getting Started

### 🔙 Backend (FastAPI)
- See [`backend/README.md`](backend/README.md)

### 🔜 Frontend (WPF)
- See [`frontend/README.md`](frontend/README.md)

---

## 🤝 Contributors

- [siva-manin](https://github.com/siva-manin)
- [uma-manin]

---

## 📄 License

This project is licensed under the MIT License.
