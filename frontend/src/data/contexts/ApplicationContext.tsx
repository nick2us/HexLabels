import { createContext } from "react";

export type Session = {
  user?: null;
  profile?: null;
};
export type SessionContextType = {
  session?: Session;
  setSession?: React.Dispatch<React.SetStateAction<Session>>;
};

type ApplicationContextType = {
  session?: SessionContextType | null;
};

export const ApplicationContext = createContext<ApplicationContextType>({});
