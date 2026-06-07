import "./App.css";
import SessionManager from "./app/SessionManager";
import { ApplicationContextProvider } from "./data/contexts/ApplicationContextProvider";
import { ThemeProvider } from "./utils/ThemeProvider";
function App() {
  return (
    <ApplicationContextProvider>
      <ThemeProvider>
        <SessionManager />
      </ThemeProvider>
    </ApplicationContextProvider>
  );
}

export default App;
