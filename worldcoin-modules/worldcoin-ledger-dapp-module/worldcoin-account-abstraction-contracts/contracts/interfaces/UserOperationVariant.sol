// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.18;

import { WorldIDVerification } from "./WorldIDVerification.sol";

struct UserOperationVariant {
    address sender;
    WorldIDVerification worldIDVerification;
    bytes callData;
    uint256 callGasLimit;
}
