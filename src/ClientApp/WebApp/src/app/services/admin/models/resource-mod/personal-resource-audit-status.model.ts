/**
 * 个人资源审核状态。
 */
export enum PersonalResourceAuditStatus {
  /** 私有资源无需审核。 */
  NotRequired = 0,
  /** 等待管理员审核。 */
  Pending = 1,
  /** 审核通过。 */
  Approved = 2,
  /** 审核驳回。 */
  Rejected = 3,
}
