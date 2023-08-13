// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.18;

import { IAccountFactory } from "./interfaces/IAccountFactory.sol";
import { Account } from "./Account.sol";

contract AccountFactory is IAccountFactory {
    address public immutable wETH;
    address public immutable worldIDRouter;

    constructor(address _wETH, address _worldIDRouter) {
        wETH = _wETH;
        worldIDRouter = _worldIDRouter;
    }

    function createAccount() external returns (address account) {
        account = address(new Account(wETH, worldIDRouter));
    }
}
