import { useEffect, useMemo, useState } from "react";

export function useClientPagination<T>(items: T[], defaultPageSize = 10) {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(defaultPageSize);

  useEffect(() => {
    setPage(0);
  }, [items.length, rowsPerPage]);

  useEffect(() => {
    const lastPage = Math.max(0, Math.ceil(items.length / Math.max(rowsPerPage, 1)) - 1);
    if (page > lastPage) setPage(lastPage);
  }, [items.length, page, rowsPerPage]);

  const pageItems = useMemo(
    () => items.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage),
    [items, page, rowsPerPage]
  );

  return {
    page,
    setPage,
    rowsPerPage,
    setRowsPerPage,
    pageItems,
    total: items.length,
  };
}
