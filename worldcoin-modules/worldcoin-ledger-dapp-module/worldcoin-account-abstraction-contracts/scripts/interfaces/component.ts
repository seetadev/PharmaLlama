export interface WorldIDVerification {
  root: number
  group: number
  signal: string
  nullifierHash: number
  appID: string
  actionID: string
  proof: number[]
}

export interface UserOperationVariant {
  sender: string
  worldIDVerification: WorldIDVerification
  callData: string
  callGasLimit: number
}
