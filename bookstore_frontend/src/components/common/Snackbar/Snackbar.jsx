import { Alert, Snackbar } from "@mui/material";

const SnackBar = ({ open, onClose, severity, message }) => {
  return (
    <Snackbar
      open={open}
      onClose={onClose}
      autoHideDuration={1000}
      anchorOrigin={{ vertical: "top", horizontal: "right" }}
    >
      <Alert onClose={onClose} severity={severity} variant="filled">
        {message}
      </Alert>
    </Snackbar>
  );
};

export default SnackBar;