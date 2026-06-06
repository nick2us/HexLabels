import "./App.css";
import { ApplicationContext } from "./data/contexts/ApplicationContext";
import { ThemeProvider } from "./utils/ThemeProvider";

function App() {
  return (
    <ApplicationContext
      value={{
        user: null,
        session: null,
        profile: null,
      }}
    >
      <ThemeProvider></ThemeProvider>
    </ApplicationContext>
  );
}

export default App;
