import { Moon, Sun } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useTheme } from "./useTheme";

export function ThemeToggler() {
  const { theme, setTheme } = useTheme();

  const toggleTheme = () => {
    if (theme === "dark") {
      setTheme("light");
    } else {
      setTheme("dark");
    }
  };

  return (
    <div className="flex mr-2">
      <Button
        variant="secondary"
        size="icon"
        onClick={toggleTheme}
        className="relative rounded-full transition-transform"
      >
        <Sun className="absolute h-[1.2rem] w-[1.2rem] transition-all duration-300 ease-in-out rotate-0 scale-100 dark:-rotate-90 dark:scale-0 text-black" />

        <Moon className="absolute h-[1.2rem] w-[1.2rem] transition-all duration-300 ease-in-out rotate-90 scale-0 dark:rotate-0 dark:scale-100 text-white" />

        <span className="sr-only">Toggle theme</span>
      </Button>
    </div>
  );
}
