import styles from "./Signup.module.scss";
import logo from "../../assets/authshopping.png";
import { Button, CircularProgress, TextField } from "@mui/material";
import { useState } from "react";
import SnackBar from "../../components/common/Snackbar/Snackbar";
import { validateField } from "../../utils/Validations";
import { useNavigate } from "react-router-dom";
import useAPI from "../../hooks/useApi";

const Signup = () => {
  const navigate = useNavigate();
  const { loading, request } = useAPI();
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
  });
  const [errors, setErrors] = useState({});
  const [snackbarData, setSnackbarData] = useState({
    open: false,
    message: "",
    severity: "error",
  });

  const handleCloseSnackbar = () => {
    setSnackbarData((prev) => ({
      ...prev,
      open: false,
    }));
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    setErrors((prev) => ({
      ...prev,
      [name]: validateField(name, value),
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const newErrors = Object.keys(formData).reduce((acc, key) => {
      acc[key] = validateField(key, formData[key]);
      return acc;
    }, {});
    setErrors(newErrors);
    const hasErrors = Object.values(newErrors).some((error) => error);
    if (hasErrors) {
      setSnackbarData({
        open: true,
        message: "Please fix the errors in the form.",
        severity: "error",
      });
      return;
    }

    try {
      await request({
        method: "POST",
        url: "/api/User/Register",
        data: formData,
        headers: {
          "Content-Type": "application/json",
        },
      });
      setSnackbarData({
        open: true,
        message: "Registration Successful!",
        severity: "success",
      });
      setTimeout(() => {
        navigate("/login");
      }, 1500);
    } catch (error) {
      setSnackbarData({
        open: true,
        message: error.response?.data?.message,
        severity: "error",
      });
    }
  };

  return (
    <>
      <div className={styles.authWrapper}>
        <div className={styles.authCardsContainer}>
          <div className={styles.leftcard}>
            <div className={styles.imageLogo}>
              <img src={logo} alt="Shopping Logo" />
            </div>
            <div className={styles.logoText}>
              <p>Online Book Shopping</p>
            </div>
          </div>
          <div className={styles.rightcard}>
            <div className={styles.formContainer}>
              <div className={styles.navLinks}>
                <p onClick={() => navigate("/login")}>login</p>
                <p className={styles.active}>signup</p>
              </div>
              <form className={styles.formBox} onSubmit={handleSubmit}>
                <TextField
                  variant="outlined"
                  label="First Name"
                  type="text"
                  value={formData.firstName}
                  onChange={handleChange}
                  name="firstName"
                  error={!!errors.firstName}
                  helperText={errors.firstName}
                />
                <TextField
                  variant="outlined"
                  label="Last Name"
                  type="text"
                  value={formData.lastName}
                  onChange={handleChange}
                  name="lastName"
                  error={!!errors.lastName}
                  helperText={errors.lastName}
                />
                <TextField
                  variant="outlined"
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  name="email"
                  error={!!errors.email}
                  helperText={errors.email}
                />
                <TextField
                  variant="outlined"
                  label="Password"
                  type="password"
                  value={formData.password}
                  onChange={handleChange}
                  name="password"
                  error={!!errors.password}
                  helperText={errors.password}
                />
                <Button
                  variant="contained"
                  color="error"
                  type="submit"
                  disabled={loading}
                >
                  {loading ? (
                    <CircularProgress
                      size={20}
                      color="inherit"
                      className={loading ? styles.disabled : ""}
                    />
                  ) : (
                    "SignUp"
                  )}
                </Button>
              </form>
            </div>
          </div>
        </div>
      </div>
      <SnackBar
        open={snackbarData.open}
        onClose={handleCloseSnackbar}
        message={snackbarData.message}
        severity={snackbarData.severity}
      />
    </>
  );
};

export default Signup;
