import { createContext } from "react";

type ApplicationContextType = {
  user: null;
  session: null;
  profile: null;
};

export const ApplicationContext = createContext<ApplicationContextType>({
  user: null,
  session: null,
  profile: null,
});
