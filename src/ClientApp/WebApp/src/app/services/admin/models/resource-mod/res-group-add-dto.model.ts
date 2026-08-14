export interface ResGroupAddDto {
  /** name */
  name: string;
  /** description */
  description?: string | null;
  /** icon */
  icon?: string | null;
  /** color */
  color: string;
  /** categoryId */
  categoryId: string;
}
