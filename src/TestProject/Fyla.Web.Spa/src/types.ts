export type DiskItem = {
    name: string;
    type: "File" | "Directory";
    size: number;
    lastModified: string;
    fullPath: string;
};
