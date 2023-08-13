// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.18;

import { IERC20 } from "@openzeppelin/contracts/token/ERC20/IERC20.sol";

import { ByteHasher } from "./libraries/ByteHasher.sol";
import { IAccount, UserOperationVariant } from "./interfaces/IAccount.sol";
import { IQuickSwapV3Router } from "./dependencies/IQuickSwapV3Router.sol";
import { IWorldIDRouter } from "./interfaces/IWorldIDRouter.sol";
import { WorldIDVerification } from "./interfaces/WorldIDVerification.sol";

contract Account is IAccount {
    using ByteHasher for bytes;

    event AccountCreated(address addr);

    address public immutable wETH;
    address public immutable worldIDRouter;

    constructor(address _wETH, address _worldIDRouter) {
        wETH = _wETH;
        worldIDRouter = _worldIDRouter;

        emit AccountCreated(address(this));
    }

    receive() external payable {}

    function validateUserOp(
        UserOperationVariant calldata op
    ) external returns (uint256 validationData) {}

    function verify(
        WorldIDVerification calldata verif
    ) external returns (bool) {
        uint256 signalHash = abi.encodePacked(verif.signal).hashToField();
        uint256 appIDHash = abi.encodePacked(verif.appID).hashToField();
        uint256 externalNullifierHash = abi
            .encodePacked(appIDHash, verif.actionID)
            .hashToField();

        try
            IWorldIDRouter(worldIDRouter).verifyProof(
                verif.root,
                verif.group, // `0` for phone and `1` for orb.
                signalHash,
                verif.nullifierHash,
                externalNullifierHash,
                verif.proof
            )
        {} catch {
            revert("Account: invalid WorldIDVerification");
        }

        return true;
    }

    function exactInputSingle(
        address router,
        uint256 amountIn,
        uint256 amountOutMin,
        address tokenIn,
        address tokenOut
    ) external payable returns (uint256 amountOut) {
        if (tokenIn != wETH) {
            IERC20(tokenIn).approve(router, amountIn);
        }

        IQuickSwapV3Router.ExactInputSingleParams
            memory params = IQuickSwapV3Router.ExactInputSingleParams({
                tokenIn: tokenIn,
                tokenOut: tokenOut,
                recipient: address(this),
                deadline: block.timestamp,
                amountIn: amountIn,
                amountOutMinimum: amountOutMin,
                limitSqrtPrice: 0
            });

        if (tokenIn != wETH) {
            amountOut = IQuickSwapV3Router(router).exactInputSingle(params);
        } else {
            amountOut = IQuickSwapV3Router(router).exactInputSingle{
                value: amountIn
            }(params);
        }
    }
}
