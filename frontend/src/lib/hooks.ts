"use client";
import { useQuery } from "@tanstack/react-query";

import { getMe, type User } from "./auth";

export function useCurrentUser() {
  return useQuery<User>({
    queryKey: ["me"],
    queryFn: getMe,
    retry: false,
    staleTime: 60_000,
  });
}
