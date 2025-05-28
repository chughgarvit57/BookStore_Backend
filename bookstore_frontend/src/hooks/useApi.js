import axios from "axios";
import { useState } from "react";

const useAPI = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [data, setData] = useState(null);

  const request = async ({
    method = "GET",
    url,
    data: body = null,
    headers = {},
  }) => {
    try {
      setLoading(true);
      setError(null);
      const response = await axios({
        method,
        url,
        data: body,
        headers,
      });
      setData(response);
      return response.data;
    } catch (err) {
      const errorMessage =
        err.response?.data?.message || err.message || "Something went wrong!";
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };
  return { data, loading, error, request };
};

export default useAPI;