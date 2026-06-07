import "./App.css";
import SessionManager from "./app/SessionManager";
import { ApplicationContextProvider } from "./data/contexts/ApplicationContextProvider";
import { ThemeProvider } from "./utils/ThemeProvider";
import { BrowserRouter } from "react-router";

function App() {
  return (
    <ApplicationContextProvider>
      <ThemeProvider>
        <BrowserRouter>
          <SessionManager />
        </BrowserRouter>
      </ThemeProvider>
    </ApplicationContextProvider>
  );
}

export default App;
