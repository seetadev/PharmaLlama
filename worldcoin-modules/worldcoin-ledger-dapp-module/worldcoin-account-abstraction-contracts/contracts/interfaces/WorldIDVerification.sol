// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.18;

struct WorldIDVerification {
    uint256 root;
    uint256 group;
    string signal;
    uint256 nullifierHash;
    string appID;
    string actionID;
    uint256[8] proof;
}
