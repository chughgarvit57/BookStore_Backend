import { Route, Routes } from "react-router-dom";
import PublicRoutes from "./routes/PublicRoutes";
import Login from "./pages/Login/Login";
import Signup from "./pages/Signup/SIgnup";
import PrivateRoutes from "./routes/PrivateRoutes";
import Home from "../src/pages/Home/Home";

const App = () => {
  return (
    <>
      <Routes>
        <Route element={<PublicRoutes />}>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Signup />} />
        </Route>
        <Route element={<PrivateRoutes />}>
          <Route path="/home" element={<Home />} />
        </Route>
      </Routes>
    </>
  );
};

export default App;
