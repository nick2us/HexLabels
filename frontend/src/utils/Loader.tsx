import { LoaderCircle } from "lucide-react";
import type { PropsWithChildren } from "react";

export default function Loader({
  children,
  loading,
}: PropsWithChildren & { loading?: boolean }) {
  if (loading === undefined) {
    loading = true;
  }

  return loading ? (
    <LoaderCircle size="34" className="animate-spin" />
  ) : (
    children
  );
}
