import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Link, useNavigate } from "react-router";
import { useState, type PropsWithChildren } from "react";
import { confirmSignUp, signUp } from "@/lib/login";
import { LoaderCircle } from "lucide-react";

type Step = "signup" | "confirm" | "done";

function Loader({
  children,
  loading,
}: PropsWithChildren & { loading: boolean }) {
  return loading ? (
    <Field>
      <LoaderCircle size="34" className="animate-spin" />
    </Field>
  ) : (
    children
  );
}

export default function SignupForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const [step, setStep] = useState<Step>("signup");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [code, setCode] = useState("");
  const [cpassword, setCpassword] = useState("");
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  const handleSignUp = async () => {
    try {
      setLoading(true);
      signUp(email, password).then(() => {
        setStep("confirm");
        setLoading(false);
      });
    } catch (err) {
      console.error(err);
    }
  };

  const handleConfirm = async () => {
    try {
      setLoading(true);
      confirmSignUp(email, code).then(() => {
        setLoading(false);
        navigate("/");
      });
    } catch (err) {
      console.error(err);
    }
  };

  const signupStep = () => (
    <>
      <div className="flex flex-col items-center gap-2 text-center">
        <h1 className="text-2xl font-bold">Create your account</h1>
        <p className="text-sm text-balance text-muted-foreground">
          Enter your email below to create your account
        </p>
      </div>
      <Loader loading={loading}>
        <Field>
          <FieldLabel htmlFor="email">Email</FieldLabel>
          <Input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            id="email"
            type="email"
            placeholder=""
            required
          />
        </Field>
        <Field>
          <Field className="grid grid-cols-2 gap-4">
            <Field>
              <FieldLabel htmlFor="password">Password</FieldLabel>
              <Input
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                id="password"
                type="password"
                required
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="confirm-password">
                Confirm Password
              </FieldLabel>
              <Input
                value={cpassword}
                onChange={(e) => setCpassword(e.target.value)}
                id="confirm-password"
                type="password"
                required
              />
            </Field>
          </Field>
          <FieldDescription>
            Must be at least 8 characters long.
          </FieldDescription>
        </Field>
        <Button
          onClick={(e) => {
            e.preventDefault();
            handleSignUp();
          }}
          type="submit"
        >
          Create Account
        </Button>
      </Loader>
    </>
  );

  const confirmStep = () => (
    <>
      <div className="flex flex-col items-center gap-2 text-center">
        <h1 className="text-2xl font-bold">Confirm your account</h1>
        <p className="text-sm text-balance text-muted-foreground">
          Enter the code recieved in your email.
        </p>
      </div>
      <Loader loading={loading}>
        <Field>
          <FieldLabel htmlFor="code">Code</FieldLabel>
          <Input
            value={code}
            onChange={(e) => setCode(e.target.value)}
            id="code"
            type="text"
            placeholder=""
            required
          />
        </Field>
        <Button
          onClick={(e) => {
            e.preventDefault();
            handleConfirm();
          }}
          type="submit"
        >
          Confirm
        </Button>
      </Loader>
    </>
  );

  const doneStep = () => <></>;

  const showStep = () => {
    switch (step) {
      case "signup":
        return signupStep();
      case "confirm":
        return confirmStep();
      case "done":
        return doneStep();
    }
  };

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card className="overflow-hidden p-0">
        <CardContent className="grid p-0 md:grid-cols-2">
          <form className="p-6 md:p-8">
            <FieldGroup>
              {showStep()}
              <FieldDescription className="text-center">
                Already have an account? <Link to="/">Sign in</Link>
              </FieldDescription>
            </FieldGroup>
          </form>
          <div className="relative hidden bg-muted md:block">
            <img
              src="https://fastly.picsum.photos/id/201/640/670.jpg?hmac=53tQ0WbVwezHvhNi0Sjuoby7DeBbKxQ0W2puU7pCfzA"
              alt="Image"
              className="absolute inset-0 h-full w-full object-cover dark:brightness-[0.2] dark:grayscale"
            />
          </div>
        </CardContent>
      </Card>
      <FieldDescription className="px-6 text-center">
        By clicking continue, you agree to our <a href="#">Terms of Service</a>{" "}
        and <a href="#">Privacy Policy</a>.
      </FieldDescription>
    </div>
  );
}
