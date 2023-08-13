import abi from "./abi/usdc_matic.json"

import Web3 from 'web3';
import HDWalletProvider from "@truffle/hdwallet-provider";

require('dotenv').config();

// const provider = new Web3.providers.HttpProvider(rpcURL|| "");

const privateKey = process.env.PRIVATE_KEY || ""
// Create web3.js middleware that signs transactions locally
// @ts-ignore
const localKeyProvider = new HDWalletProvider({
 privateKeys: [privateKey],
 providerOrUrl: process.env.POLYGON_RPC_URL,
});
const web3 = new Web3(localKeyProvider as any);

const myAccount = web3.eth.accounts.privateKeyToAccount(privateKey);

// Interact with existing, already deployed, smart contract on Ethereum mainnet
// const address = process.env.PUBLIC_KEY;

export async function getBalanceOf(address: string, tokenAddress: string) {
    const myContract = new web3.eth.Contract(abi as any, tokenAddress);
    console.log('Transaction signer account is', myAccount.address, ', smart contract is', tokenAddress);

    console.log('Starting transaction now');
    // Approve this balance to be used for the token swap
    const receipt = await myContract.methods.balanceOf(tokenAddress).send({
        from: myAccount.address,
        gas:"20000000", gasPrice:"2000000000",
    });
    console.log('TX receipt', receipt);
}

