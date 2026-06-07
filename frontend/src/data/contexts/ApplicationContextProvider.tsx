import { useState, type PropsWithChildren } from "react";
import { ApplicationContext, type Session } from "./ApplicationContext";

export const ApplicationContextProvider = ({ children }: PropsWithChildren) => {
  const [session, setSession] = useState<Session>({
    user: null,
    profile: null,
  });

  return (
    <ApplicationContext.Provider value={{ session: { session, setSession } }}>
      {children}
    </ApplicationContext.Provider>
  );
};
