export const API_GATEWAY_BASE_URL = 'https://israel-apigateway.onrender.com/api/v1/israel-hernandez';
export const USUARIOS_API_BASE_URL = 'https://israel-usuarios-api.onrender.com/api/v1';
export const ALOJAMIENTOS_API_BASE_URL = 'https://israel-alojamientos-api.onrender.com/api/v1';
export const RESERVAS_API_BASE_URL = 'https://israel-reservas-api.onrender.com/api/v1';
export const FACTURACION_API_BASE_URL = 'https://israel-facturacion-api.onrender.com/api/v1';
export const PARTNER_API_BASE_URL = ALOJAMIENTOS_API_BASE_URL;

// Subida directa unsigned desde Angular hacia Cloudinary.
// El preset debe existir en Cloudinary como "Unsigned" y restringido a imagenes.
export const CLOUDINARY_CLOUD_NAME = 'dzxkufyfp';
export const CLOUDINARY_UPLOAD_PRESET = 'locushome_uploads';
export const CLOUDINARY_DIRECT_UPLOAD_URL = `https://api.cloudinary.com/v1_1/${CLOUDINARY_CLOUD_NAME}/image/upload`;
