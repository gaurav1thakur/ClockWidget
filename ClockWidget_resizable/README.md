# 🕒 ClockWidget  
---

**A resizable, theme‑aware desktop clock with focus timer, system tray controls, and overlay mode.**  

---

## 📌 Overview  
ClockWidget is a lightweight WPF desktop application that displays an analog clock with optional date, focus session tracking, and customizable appearance. It integrates with the Windows system tray for quick access to settings and supports dark/light themes with live switching.  

---

## ✨ Features  
- **Analog Clock** — Smooth hands with optional glow in dark mode  
- **Focus Timer** — Circular progress arc with blinking countdown in final 10%  
- **Theme Switching** — Light/Dark themes with centralized resource management  
- **Overlay Mode** — Click‑through, always‑on‑top display  
- **System Tray Menu** — Change size, theme, and settings without opening the main window  
- **Persistent Settings** — All preferences saved between sessions  
- **Optional Date Display** — Toggle day/date on the clock face  
- **Seconds Hand Toggle** — Show/hide seconds hand via settings  
- **Future‑Ready** — Hooks for weather, sunrise/sunset, and multiple time zones  

---

## 🗂 Project Structure  

| Path / File                          | Purpose |
|--------------------------------------|---------|
| `ClockWindow.xaml`                   | Main clock UI layout and visuals |
| `ClockWindow.xaml.cs`                | Clock logic, hand movement, focus arc updates |
| `SettingsWindow.xaml`                | User settings UI |
| `SettingsWindow.xaml.cs`             | Settings logic, config updates |
| `Themes/LightTheme.xaml`             | Light mode brushes, effects, and styles |
| `Themes/DarkTheme.xaml`              | Dark mode brushes, effects, and styles |
| `Helpers/ThemeManager.cs`            | Centralized theme switching |
| `Helpers/TrayIconHelper.cs`          | WinForms‑based tray icon integration |
| `Helpers/Win32Helper.cs`             | Click‑through mode handling |
| `Config.cs`                          | Serializable user settings |
| `App.xaml` / `App.xaml.cs`           | Application startup, tray menu setup |

---

## 🛠 Setup & Run  

1. **Clone the repository**  
   ```bash
   git clone https://github.com/your-org/ClockWidget.git
   ```

2. **Open in Visual Studio**  
   - Target: `.NET 6.0` or `.NET Framework 4.8` (depending on your branch)  
   - Ensure `Themes/*.xaml` files have **Build Action** = `Resource`

3. **Build & Run**  
   - Press `F5` to start  
   - Clock appears centered on screen  
   - Right‑click tray icon for quick settings

---

## ⚙ Configuration  

Settings are stored in `Config.Current` and persisted via `Config.Current.Save()`.

| Setting                  | Description |
|--------------------------|-------------|
| `Theme`                  | `"Light"` or `"Dark"` |
| `ClockOpacity`           | 0.3 – 1.0 |
| `ClickThroughEnabled`    | `true` / `false` |
| `ClockSize`              | `"Small"`, `"Medium"`, `"Large"` |
| `IsOverlayMode`          | Overlay always‑on‑top mode |
| `ShowAnalogClock`        | Show/hide analog hands |
| `ShowDate`               | Show/hide date text |
| `ShowSecondsHand`        | Show/hide seconds hand |

---

## 🎨 Theming  

- **DynamicResource** is used for all theme‑aware brushes and effects  
- Both `LightTheme.xaml` and `DarkTheme.xaml` must define the same keys  
- Example brush:  
  ```xml
  <SolidColorBrush x:Key="ClockTextBrush" Color="Black" />
  ```

---

## 🛣 Roadmap Feature Specification

| Feature | Description | Dependencies | Status |
|---------|-------------|--------------|--------|
| **Sunrise/Sunset with Visuals** | Fetch sunrise and sunset times from a reliable API (e.g., Sunrise‑Sunset.org, OpenWeather). Display small icons or gradient background changes on the dial. Load only if data is available; otherwise leave blank. | Internet access, API key (if required), JSON parsing, theme‑aware visuals | Planned |
| **Day & Date on Watch** | Show today’s day and date (e.g., `TUE 16`) on the clock face. Text color adapts to theme. | Theme brush (`ClockTextBrush`), `DateTime.Now` formatting, DynamicResource binding | In Progress |
| **Multiple Watches / Time Zones** | Allow multiple clock instances for different time zones, each with its own size, theme, and position. | Window management, time zone conversion, config persistence | Planned |
| **Watch Faces** | Support multiple analog face designs (minimal, classic, modern) selectable in settings with live preview. | Resource dictionaries for faces, settings UI update, theme integration | Planned |
| **Seconds Hand Toggle** | Add a setting to show/hide the seconds hand. Persist in config and apply instantly. | Config flag, UI binding, visibility toggling in `ClockWindow` | Planned |
| **Settings Sync with Config** | Ensure all settings in the UI reflect `Config` values on load, with two‑way binding where possible. | Config load/save, data binding | In Progress |
| **Weather + Temperature Icon** | Fetch current weather and temperature from an API (e.g., OpenWeather). Show small icon (sun, cloud, rain, etc.) and temperature on the dial. Load only if data is available; otherwise leave blank. | Internet access, API key, JSON parsing, icon set, theme‑aware visuals | Planned |

---

## 🖼 Architecture Overview

```
                   ┌───────────────────────────┐
                   │         Config.cs         │
                   │  - Stores user settings   │
                   │  - Loads/Saves to disk    │
                   └─────────────┬─────────────┘
                                 │
                                 ▼
┌───────────────────────────────────────────────────────────────────┐
│                           App.xaml.cs                             │
│  - Startup sequence                                               │
│  - Creates ClockWindow                                            │
│  - Initializes TrayIconHelper                                     │
│  - Applies theme from Config                                      │
└─────────────┬─────────────────────────────────────────────────────┘
              │
              ▼
┌───────────────────────────┐       ┌───────────────────────────────┐
│     ClockWindow.xaml      │       │    SettingsWindow.xaml        │
│  - Analog clock UI        │       │  - UI for changing settings   │
│  - Date/Day text          │       │  - Two-way bind to Config     │
│  - Focus arc + animations │       │  - Theme-aware controls       │
└─────────────┬─────────────┘       └───────────────┬───────────────┘
              │                                     │
              ▼                                     │
┌───────────────────────────┐                       │
│   ThemeManager / Themes   │                       │
│  - LightTheme.xaml        │                       │
│  - DarkTheme.xaml         │                       │
│  - DynamicResource brushes│                       │
└─────────────┬─────────────┘                       │
              │                                     │
              ▼                                     │
┌───────────────────────────┐                       │
│   TrayIconHelper.cs       │◄ ─────────────────────┘
│  - WinForms tray menu     │
│  - Menu items for size,   │
│    theme, settings, exit  │
└───────────────────────────┘
```

---

## 📢 Contribution Notes
- **Theme Awareness**: All new visuals must use `DynamicResource` so they adapt to theme changes.  
- **Fail‑Safe UI**: If API data is missing or fails to load, the UI should gracefully hide that element without errors.  
- **Config Persistence**: Every new setting should be stored in `Config` and restored on startup.  
- **Modularity**: Keep new features in separate helper classes or resource dictionaries for maintainability.  

---
