import { LoginForm } from "@/components/login-form";
import { ApplicationContext } from "@/data/contexts/ApplicationContext";
import { Sidebar } from "@/layout/Sidebar";
import { useContext, useMemo } from "react";

export default function SessionManager() {
  const { session } = useContext(ApplicationContext);

  const isLoggedIn = useMemo(() => {
    if (session == null) {
      return false;
    }

    if (session.session == null) {
      return false;
    }

    if (session.session.user == null) {
      return false;
    }

    return true;
  }, [session]);

  return isLoggedIn ? <LoggedIn /> : <LoggedOut />;
}

function LoggedIn() {
  return (
    <>
      <Sidebar>
        <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
          <div className="grid auto-rows-min gap-4 md:grid-cols-3">
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
          </div>
          <div className="min-h-screen flex-1 rounded-xl bg-muted/50 md:min-h-min" />
        </div>
      </Sidebar>
    </>
  );
}

function LoggedOut() {
  return (
    <>
      <div className="flex min-h-svh flex-col items-center justify-center bg-muted p-6 md:p-10">
        <div className="w-full max-w-sm md:max-w-4xl">
          <LoginForm />
        </div>
      </div>
    </>
  );
}
