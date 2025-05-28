import { Navigate, Outlet } from "react-router-dom";

export const IsAuthenticated = () => {
  return localStorage.getItem("user") !== null;
};

const PublicRoutes = () => {
  return !IsAuthenticated() ? <Outlet /> : <Navigate to="/home" />;
};

export default PublicRoutes;
