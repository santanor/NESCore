const Menu  = require("electron-create-menu")
const { app, BrowserWindow } = require('electron');
const path = require("path");
const isDev = require("electron-is-dev");
const IpcHandler = require('./ipcHandler')

/**
 * Holds a reference to all the windows that the app can have 
 */
let windows = {
    mainWindow: null,
    nametableViewerWindow: null,
}


/**
 * Creates the main window, this function will save the window reference in the "windows" variable
 */
function createMainWindow() {
    windows.mainWindow = new BrowserWindow({ width: 160, height: 144, webPreferences:{nodeIntegration: true} });
    windows.mainWindow.loadURL(
        isDev
            ? "http://localhost:3000/game"
            : `file://${path.join(__dirname, "../build/index.html/game")}`
    );
    windows.mainWindow.on("closed", () => (windows.mainWindow = null));
    setMainMenu();
}

/**
 * Creates the memory viewer window
 */
function createNametableViewerWindow(){
    windows.nametableViewerWindow = new BrowserWindow({ width: 300, height: 700, webPreferences:{nodeIntegration: true} });
    windows.nametableViewerWindow.loadURL(
        isDev
            ? "http://localhost:3000/memoryViewer"
            : `file://${path.join(__dirname, "../build/index.html/memoryViewer")}`
    );
    windows.nametableViewerWindow.on("closed", () => (windows.memoryViewerWindow = null));
}

app.on("ready", createMainWindow);
app.on("window-all-closed", () => {
    if (process.platform !== "darwin") {
        app.quit();
    }
});
app.on("activate", () => {
    if (windows.mainWindow === null) {
        createMainWindow();
    }
});

function setMainMenu() {
    Menu();

    Menu((defaultMenu, separator) => {
 
        defaultMenu.push({
          label: 'Debugger',
          submenu: [
            {label: 'Nametable Viewer', click:  () =>createNametableViewerWindow},
            separator()
          ],
        });
       
        return defaultMenu;
      });
}

module.exports = windows;


