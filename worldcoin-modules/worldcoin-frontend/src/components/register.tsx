import { useState, CSSProperties } from "react";
import Background from '../layouts/background'
import HashLoader from "react-spinners/HashLoader";
import { useNavigate } from "react-router-dom";

const override: CSSProperties = {
  display: "block",
  margin: "0 auto",
  borderColor: "red",
};

export default function Register() {
  const navigation = useNavigate();
  const [loading, setLoading] = useState(true);

  setTimeout(() => {
    navigation('/register/success')
  }, 3000)

  return (
    <Background>
      <div className="flex flex-col text-center">
        <div className="space-y-2 flex flex-col items-center p-8">
          <h3 className="text-3xl text-gray-700 font-semibold text-center">
            Register 🚀
          </h3>
        </div>
        <div className="my-40">
          <HashLoader
            color={"#ffabf9"}
            loading={loading}
            cssOverride={override}
            size={100}
            aria-label="Loading Spinner"
            data-testid="loader"
          />
        </div>
      </div>
    </Background>
  )
}
