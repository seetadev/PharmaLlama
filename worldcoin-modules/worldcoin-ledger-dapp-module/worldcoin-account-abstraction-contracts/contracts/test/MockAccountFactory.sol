// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.18;

import { Account } from "../Account.sol";
import { IAccountFactory } from "../interfaces/IAccountFactory.sol";

contract MockAccountFactory is IAccountFactory {
    address public immutable wETH;
    address public immutable worldIDRouter;

    address public lastAccount;

    constructor(address _wETH, address _worldIDRouter) {
        wETH = _wETH;
        worldIDRouter = _worldIDRouter;
    }

    function createAccount() external returns (address account) {
        account = address(new Account(wETH, worldIDRouter));

        lastAccount = account;
    }

    function getLastAccount() external view returns (address) {
        return lastAccount;
    }
}
