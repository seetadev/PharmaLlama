'use client'
import Background from '../layouts/background'
import { useNavigate } from "react-router-dom";
import { useEffect, useState } from 'react';

export default function Home() {
  // const params = useSearchParams();
  // const action = params.get('action');
  // const appID = params.get('app_id');
  // const signal = params.get('signal');
  // const credentialType = params.get('credential_type');
  // const nullifierHash = params.get('nullifier_hash');
  // const proof = params.get('proof');
  // const merkleRoot = params.get('merkle_root');

  // console.log(merkleRoot, proof)
  // const {
  //   action,
  //   app_id: appID,
  //   signal,
  //   credential_type: credentialType,
  //   nullifier_hash: nullifierHash,
  //   proof,
  //   merkle_root
  // } = params;

  // console.log(proof)
  const [credential, setCredential] = useState()
  const [aaAddress, setAAAddress] = useState("")

  const navigate = useNavigate();

  useEffect(() => {
    if (credential) return
    const credRaw = localStorage.getItem("cred")
    if (!credRaw) return
    const cred = JSON.parse(credRaw)
    setCredential(cred)
  }, [credential])

  useEffect(() => {
    if (aaAddress) return
    const address = localStorage.getItem("aa_address")
    if (!address) return
    setAAAddress(address)
  }, [aaAddress])

  const onLogOut = () => {
    localStorage.clear()
    navigate('/')
  }

  const onClickRegister = () => {
    navigate('/register')
  }

  const onClickTransfer = (chain: "polygon" | "gnosis" | "near") =>  {
    navigate('/transfer', {state:{chain:chain}})
  }

  return (
    <Background>
      <div className="flex flex-col text-center">
        <div className="space-y-2 flex flex-col items-center p-8">
          <h3 className="text-3xl text-gray-700 font-semibold text-center pb-4">
          Cross-chain <br/> Worldcoin 👀 <br/> AA Wallet
          </h3>
          {/* <div>
            <span className='font-semibold'>Address: </span><span>{aaAddress || "Please register an AA wallet"}</span>
          </div> */}
          <button onClick={onLogOut} className="py-4 items-center w-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300">
            <span className="text-sm">Log Out</span>
          </button>
          {/* <h5 className="text text-gray-500 font text-center">
          Bye bye private keys! 👋 <br /> <br />Chain-agnostic account abstraction wallet using Worldcoin.
          </h5> */}
        </div>
        <button className="py-4 h-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300" onClick={onClickRegister}>
          <span className="text-2xl">Register</span>
        </button>
        <button className="py-4 h-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300" onClick={() => onClickTransfer("polygon")}>
          <span className="text-2xl">Swap on Polygon</span>
        </button>
        <button className="py-4 h-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300" onClick={() => onClickTransfer("gnosis")}>
          <span className="text-2xl">Swap on Gnosis Chain <br />(via bridge)</span>
        </button>
        <button className="py-4 h-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300" onClick={() => onClickTransfer("near")}>
          <span className="text-2xl">Swap on Aurora / Near <br />(via bridge)</span>
        </button>
        <button className="py-4 h-1/3 border-4 rounded-2xl hover:bg-pink-100 border-pink-300" onClick={() => onClickTransfer("near")}>
          <span className="text-2xl">Custom Invocation <br />(Calldata)</span>
        </button>
      </div>
    </Background>
  )
}
