// SPDX-License-Identifier: MIT
pragma solidity ^0.8.18;

library ByteHasher {
    function hashToField(bytes memory value) internal pure returns (uint256) {
        return uint256(keccak256(abi.encodePacked(value))) >> 8;
    }
}
