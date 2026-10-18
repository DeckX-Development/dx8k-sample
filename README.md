# DeckX Sample Templates (dx8k-sample)

Official developer sample templates for **DeckX Control Studio** and DeckX devices (.NET 8).

This repository contains ready-to-use template projects and developer guides for building plugins, canvas widgets, clock faces, and game overlays for DeckX.

---

## 📦 Included Templates

| Template | Type | Description | Deep Dive Guide |
| :--- | :--- | :--- | :--- |
| [**DX8K.Plugin.SampleTemplate**](./DX8K-Sample-Windows/DX8K.Plugin.SampleTemplate) | 🔲 **Button Plugin** | 8-Key hardware button plugin template demonstrating dynamic button actions, two-way state toggles, click counters, custom borders, and live telemetry tickers. | [Developer Guide](./DX8K-Sample-Windows/DX8K.Plugin.SampleTemplate/DEVELOPER_GUIDE.md) |
| [**DX8K.Plugin.CanvasSampleTemplate**](./DX8K-Sample-Windows/DX8K.Plugin.CanvasSampleTemplate) | 🎨 **Canvas Plugin** | Full-canvas (480x272) rendering plugin template for rich custom UI, graphics, and capacitive touch controls. | [Developer Guide](./DX8K-Sample-Windows/DX8K.Plugin.CanvasSampleTemplate/DEVELOPER_GUIDE.md) |
| [**DX8K.ClockFace.SampleTemplate**](./DX8K-Sample-Windows/DX8K.ClockFace.SampleTemplate) | ⏰ **Clock Face** | Custom standby/active digital & analog clock face template for DeckX. | [Developer Guide](./DX8K-Sample-Windows/DX8K.ClockFace.SampleTemplate/DEVELOPER_GUIDE.md) |
| [**DX8K.GameOverlay.SampleTemplate**](./DX8K-Sample-Windows/DX8K.GameOverlay.SampleTemplate) | 🎮 **Game Overlay** | Full-screen in-game monitoring and hardware telemetry HUD overlay template. | [Developer Guide](./DX8K-Sample-Windows/DX8K.GameOverlay.SampleTemplate/DEVELOPER_GUIDE.md) |

---

## 🛠️ Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Visual Studio 2022 / VS Code / JetBrains Rider
- [DeckX Control Studio](https://github.com/Nactro-Development/DeckX-Control-Studio)

---

## 🚀 Getting Started

1. Clone this repository:
   ```bash
   git clone https://github.com/DeckX-Development/dx8k-sample.git
   ```
2. Navigate to the desired template folder inside `DX8K-Sample-Windows`:
   ```bash
   cd dx8k-sample/DX8K-Sample-Windows/DX8K.Plugin.SampleTemplate
   ```
3. Check the template's `README.md` and included `DEVELOPER_GUIDE.md` for detailed build and setup instructions.
