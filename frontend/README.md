## 📄 `frontend/README.md`

**`/frontend/README.md`**

```markdown
# 🖥️ WPF Frontend — HospitalAuthSystem

This is the secure kiosk-mode login shell for Windows, built using WPF (C#). It provides both facial and credential login.

---

## 🛠️ Requirements

- .NET 6+ SDK
- Visual Studio 2022 or later
- OpenCvSharp4
- Newtonsoft.Json

---

## ⚙️ Setup Instructions

1. Open `HospitalLoginApp.sln` in Visual Studio
2. Restore NuGet packages (auto or via right-click > Restore)
3. Build the project

---

## 🚀 Features

- Toggle between credential and face login
- Live webcam preview using OpenCvSharp
- Calls FastAPI backend via HTTP
- Fullscreen kiosk lock with no Start menu or Alt+Tab
- Auto-launch on system boot

---

## 💡 Notes

- On successful login, the app re-enables Windows shell (`explorer.exe`)
- Last successful login is cached for offline use
- Ensure Wi-Fi is auto-connected at startup

---

## 🔗 API Endpoints Used

- `POST /login/`
- `POST /verify/`
- `POST /register/`

---

## 🧪 Testing the App

- Test via `MainWindow.xaml` UI
- Or build `.exe` and test inside a VM
- Supports shell replacement via registry

---

## 🧰 Developer Notes

- Webcam logic: `WebcamHelper.cs`
- HTTP calls: `ApiService.cs`
- Shell restore: `ShellHelper.cs`
