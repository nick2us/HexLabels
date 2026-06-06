import { createContext } from "react";

type ThemeType = {
  theme: string;
  toggleTheme: () => void;
  setTheme: React.Dispatch<React.SetStateAction<string>>;
};

export const ThemeContext = createContext<ThemeType | null>(null);
