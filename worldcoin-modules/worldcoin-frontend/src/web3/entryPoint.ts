import HDWalletProvider from "@truffle/hdwallet-provider";
import { BigNumber } from "ethers";
import { Dispatch, SetStateAction } from "react";
import Web3 from "web3";
import entry_point_abi from "./abi/entry_point.json";
import abi from "./abi/exactInputSingle.json";

require("dotenv").config();

const privateKey = process.env.PRIVATE_KEY || "";
const localKeyProvider = new HDWalletProvider({
  privateKeys: [privateKey],
  providerOrUrl: process.env.POLYGON_RPC_URL,
});
const web3 = new Web3(localKeyProvider as any);

const myAccount = web3.eth.accounts.privateKeyToAccount(privateKey);
const entryPointContractAddress = "0x7CaB4088B48a5010e65ad70C7C546372BA7E55DB";

interface WorldIDVerification {
  root: BigNumber;
  group: BigNumber;
  signal: string;
  nullifierHash: BigNumber;
  appID: string;
  actionID: string;
  proof: BigNumber[];
}

interface UserOperationVariant {
  sender: string;
  worldIDVerification: WorldIDVerification;
  callData: string;
  callGasLimit: number;
}

export async function callExactInputSingle(
  router: string,
  amountIn: string,
  amountOutMin: string,
  tokenIn: string,
  tokenOut: string,
  setTransaction: Dispatch<SetStateAction<string>>
) {
  const entryPointContract = new web3.eth.Contract(
    entry_point_abi as any,
    entryPointContractAddress
  );

  // Access the 'cred' data from localStorage
  // const credString = localStorage.getItem('cred');

  // Parse the JSON string into an object.
  // const cred = JSON.parse(credString || '');
  const cred = {
    merkle_root: BigNumber.from(
      "21651615895955828151852809722783968631497059613185788632496826434461907998126"
    ),
    group: BigNumber.from("1"),
    signal: "",
    nullifier_hash: BigNumber.from(
      "6728982073551526785722952266936853582119478438086215932478760190917718952680"
    ),
    app_id: "app_574b973f44f8e4ce8aef8b29c16aea75",
    action: "signup",
    proof: [
      BigNumber.from(
        "2333333733273306321795567365140581065672082323104646785305898632316171833013"
      ),
      BigNumber.from(
        "5875327076857389847128448536109815438269429571993340134280193209652596596397"
      ),
      BigNumber.from(
        "9106708784576333189079679655038633754829918750642832708423659586049166856705"
      ),
      BigNumber.from(
        "19322322736555249354894479029874230118337796974345692395729723741384989124824"
      ),
      BigNumber.from(
        "15295043829978438530774756459723211257521998509377547480388973668481728868989"
      ),
      BigNumber.from(
        "8326951365504379002512535735692255267639097398089148187494423941235589693195"
      ),
      BigNumber.from(
        "8909247863636284552005463819141545643760988818720908876848215937057730927654"
      ),
      BigNumber.from(
        "2175797585662931625218291133743597373136042265748485647914138129379943746143"
      ),
    ],
  };

  console.log(cred);

  const worldIDVerif: WorldIDVerification = {
    root: cred.merkle_root,
    group: cred.group, // update this line accordingly, as the group property is not present in the provided cred object
    signal: cred.signal,
    nullifierHash: cred.nullifier_hash,
    appID: cred.app_id,
    actionID: cred.action,
    proof: cred.proof, // make sure to convert this to the correct format if it's not a number array
  };

  console.log("Starting transaction now");

  console.log("worldIdVerif", worldIDVerif);

  const gasPrice = await web3.eth.getGasPrice();

  console.log("gasPrice", gasPrice);

  const walletContractAddress = "0xa5e508f6c18a1b88db465602b2488794991ec247";
  const walletContract = new web3.eth.Contract(
    abi as any,
    walletContractAddress
  );

  const encodedCallData = walletContract.methods
    .exactInputSingle(router, amountIn, amountOutMin, tokenIn, tokenOut)
    .encodeABI();

  console.log("encodedCallData", encodedCallData);

  //   const receipt = await web3.eth.sendTransaction({
  //     from: myAccount.address,
  //     to: entryPointContractAddress,
  //     data: encodedCallData,
  //     gas: 13_000_000,
  //     gasPrice: gasPrice,
  //   });

  //   console.log('TX receipt', receipt);

  const userOpCallSwapFunc: UserOperationVariant = {
    sender: "0xa5e508F6C18A1B88Db465602B2488794991eC247",
    worldIDVerification: worldIDVerif,
    callData: encodedCallData,
    callGasLimit: 2_500_000,
  };

  console.log("myAccount.address", myAccount.address);

  // const entryPointContractAddress = ""
  // const entryPointContract = new web3.eth.Contract(abi as any, entryPointContractAddress);

  const entryPointCalldata = entryPointContract.methods
    .handleOps([userOpCallSwapFunc])
    .encodeABI();

  // 159662720585 * 3000000
  // 0.454256772009742581 * 1e18

  try {
    const receipt = await web3.eth.sendTransaction({
      from: myAccount.address,
      to: entryPointContractAddress,
      data: entryPointCalldata,
      gas: userOpCallSwapFunc.callGasLimit,
      gasPrice: gasPrice,
    });

    console.log("TX receipt", receipt);
    setTransaction(receipt.transactionHash);
  } catch (e) {
    console.log("error", e);
  }
}
