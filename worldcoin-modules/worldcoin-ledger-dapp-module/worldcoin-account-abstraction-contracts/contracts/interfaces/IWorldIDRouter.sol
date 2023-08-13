// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.18;

interface IWorldIDRouter {
    function verifyProof(
        uint256 groupId,
        uint256 root,
        uint256 signalHash,
        uint256 nullifierHash,
        uint256 externalNullifierHash,
        uint256[8] calldata proof
    ) external;
}
