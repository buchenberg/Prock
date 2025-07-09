
export interface AsyncData<T> {
    isLoading: boolean;
    isError: boolean;
    errorMessage?: string;
    value?: T;
}
