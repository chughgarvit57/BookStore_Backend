import "./Input.module.scss";

const Input = ({ type, placeholder, name, onChange, value }) => {
  return (
    <input
      type={type}
      placeholder={placeholder}
      value={value}
      name={name}
      onChange={onChange}
    />
  );
};

export default Input;
