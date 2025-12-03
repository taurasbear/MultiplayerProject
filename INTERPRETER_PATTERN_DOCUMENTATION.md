# 🎯 **INTERPRETER PATTERN - COMMAND REFERENCE**

## **📋 Available Commands (12 Total)**

### **⚡ Activation**
- Press **`~`** or **`/`** to toggle command mode
- Type command and press **Enter** to execute
- Press **Escape** to cancel
- Activation keys won't appear in command input

### **👥 Player Commands (3)**
| Command | Usage | Description |
|---------|-------|-------------|
| `/kick` | `/kick PlayerName` | Remove player from server |
| `/list` | `/list` | Show connected players |
| `/info` | `/info PlayerName` | Show player details |

### **🎮 Game Commands (4)**
| Command | Usage | Description |
|---------|-------|-------------|
| `/stats` | `/stats` | Show game statistics |
| `/set_score` | `/set_score PlayerName 100` | Set player score |
| `/restart` | `/restart` | Reset game scores |
| `/trigger_stats` | `/trigger_stats` | Log visitor statistics |

### **👾 Enemy Commands (3)**
| Command | Usage | Description |
|---------|-------|-------------|
| `/spawn` | `/spawn bird 400 300` | Spawn enemy at coordinates |
| `/clear` | `/clear` | Remove all enemies |
| `/spawn_rate` | `/spawn_rate 2.5` | Set enemy spawn interval |

### **🔧 Server Commands (2)**
| Command | Usage | Description |
|---------|-------|-------------|
| `/shutdown` | `/shutdown` | Stop server |
| `/help` | `/help` | Show this help |

---

## **🎯 Enemy Types**
- `bird` - Basic flying enemy
- `blackbird` - Fast flying enemy  
- `mine` - Stationary explosive

## **💡 Example Usage**
```
~ (activate)
list
stats
spawn bird 400 300
kick BadPlayer
shutdown
```

**Simple multiplayer server administration with 12 essential commands!**
