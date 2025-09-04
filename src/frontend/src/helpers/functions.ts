const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString() + ' ' + new Date(dateString).toLocaleTimeString();
};

export { formatDate };