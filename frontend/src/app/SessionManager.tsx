import { ApplicationContext } from "@/data/contexts/ApplicationContext";
import { lazy, Suspense, useContext, useMemo } from "react";
import { Route, Routes } from "react-router";
import Loader from "@/utils/Loader";

const LoginForm = lazy(() => import("@/components/login-form"));
const SignupForm = lazy(() => import("@/components/signup-form"));
const Sidebar = lazy(() => import("@/layout/Sidebar"));

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
      <Routes>
        <Route
          path="/"
          element={
            <Suspense fallback={<Loader />}>
              <div className="flex min-h-svh flex-col items-center justify-center bg-muted p-6 md:p-10">
                <div className="w-full max-w-sm md:max-w-4xl">
                  <LoginForm />
                </div>
              </div>
            </Suspense>
          }
        />
        <Route
          path="/signup"
          element={
            <Suspense fallback={<Loader />}>
              <div className="flex min-h-svh flex-col items-center justify-center bg-muted p-6 md:p-10">
                <div className="w-full max-w-sm md:max-w-4xl">
                  <SignupForm />
                </div>
              </div>
            </Suspense>
          }
        />
      </Routes>
    </>
  );
}
