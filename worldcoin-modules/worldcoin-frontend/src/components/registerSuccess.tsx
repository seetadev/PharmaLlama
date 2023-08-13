import { useNavigate } from 'react-router-dom'
import Background from '../layouts/background'
import { useEffect } from 'react';

const DeployedAddress = "0xcCC05d9631e7B0F1E5629A62E79A9F1C84ad5dC5"

export default function RegisterSuccess() {
  const navigation = useNavigate();
  // const [aaAddress, setAAAddress] = useState()

  const onClickHome = () => {
    navigation('/execute')
  }

  useEffect(() => {
    // if (aaAddress) return
    localStorage.setItem("aa_address", DeployedAddress)
   })

  return (
    <Background>
      <div className="flex flex-col text-center">
        <div className="space-y-2 flex flex-col items-center p-8">
          <h3 className="text-3xl text-gray-700 font-semibold text-center">
            Success 🎉
          </h3>
        </div>
        <p className='text-8xl my-40'>✔️</p>
        <button className="py-8 h-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300" onClick={onClickHome}>
          <span className="text-2xl">Back to home</span>
        </button>
      </div>
    </Background>
  )
}
