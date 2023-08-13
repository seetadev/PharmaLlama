import "@nomicfoundation/hardhat-chai-matchers";
import "@nomiclabs/hardhat-ethers";
import "@nomiclabs/hardhat-etherscan";
import "@typechain/hardhat";
import * as dotenv from "dotenv";
import "hardhat-change-network";
import { HardhatUserConfig } from "hardhat/config";

dotenv.config();

const config: HardhatUserConfig = {
  solidity: {
    version: "0.8.18",
    settings: {
      optimizer: {
        enabled: true,
        runs: 1000,
      },
    },
  },
  paths: {
    sources: "./contracts",
    cache: "./cache",
    artifacts: "./artifacts",
  },
  networks: {
    hardhat: {
      allowUnlimitedContractSize: true,
      blockGasLimit: 30_000_000,
      forking: {
        url: `${process.env.POLYGON_RPC_URL}`,
        enabled: true,
        blockNumber: 43491969,
      },
    },
    localhost: {
      url: "http://127.0.0.1:8545/",
    },
    polygon: {
      url: `${process.env.POLYGON_RPC_URL}`,
      chainId: 137,
      accounts: [`0x${process.env.PRIVATE_KEY}`],
    },
  },
};

export default config;
