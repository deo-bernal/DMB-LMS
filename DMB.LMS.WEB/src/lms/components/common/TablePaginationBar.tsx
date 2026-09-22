import type { ChangeEvent } from "react";

export type TablePaginationBarProps = {
  page: number;
  rowsPerPage: number;
  total: number;
  rowsPerPageOptions?: number[];
  onPageChange: (page: number) => void;
  onRowsPerPageChange: (rowsPerPage: number) => void;
};

export default function TablePaginationBar({
  page,
  rowsPerPage,
  total,
  rowsPerPageOptions = [10, 25, 50],
  onPageChange,
  onRowsPerPageChange,
}: TablePaginationBarProps) {
  const from = total === 0 ? 0 : page * rowsPerPage + 1;
  const to = Math.min(total, (page + 1) * rowsPerPage);
  const lastPage = Math.max(0, Math.ceil(total / Math.max(rowsPerPage, 1)) - 1);

  return (
    <div className="table-pagination" role="navigation" aria-label="Table pagination">
      <label className="table-pagination-size">
        Rows per page:
        <select
          value={rowsPerPage}
          onChange={(event: ChangeEvent<HTMLSelectElement>) => {
            onRowsPerPageChange(Number(event.target.value));
          }}
        >
          {rowsPerPageOptions.map((option) => (
            <option key={option} value={option}>
              {option}
            </option>
          ))}
        </select>
      </label>
      <span className="table-pagination-range">
        {from}–{to} of {total}
      </span>
      <div className="table-pagination-actions">
        <button type="button" aria-label="Previous page" disabled={page <= 0} onClick={() => onPageChange(page - 1)}>
          ‹
        </button>
        <button type="button" aria-label="Next page" disabled={page >= lastPage} onClick={() => onPageChange(page + 1)}>
          ›
        </button>
      </div>
    </div>
  );
}
