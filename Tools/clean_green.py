# Tools/clean_green.py
"""
Herramienta de limpieza de fondo verde chroma y eliminación de halos verdes (despill)
para las hojas de sprites de La Llorona del Callejón.
"""
import os
import glob
from PIL import Image

def clean_image(image_path):
    print(f"Procesando: {image_path}")
    img = Image.open(image_path).convert("RGBA")
    width, height = img.size
    pixels = img.load()

    cleaned_pixels = 0
    despilled_pixels = 0

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue

            # 1. Detección de fondo verde puro o casi puro (Chroma Key #00FF00)
            # Verde dominante con alta saturación
            is_pure_green = (g > 150 and g > r * 1.4 and g > b * 1.4) or (g > 200 and r < 80 and b < 80)
            is_chroma_green = (g > 120 and r < 70 and b < 70)

            if is_pure_green or is_chroma_green:
                pixels[x, y] = (0, 0, 0, 0)
                cleaned_pixels += 1
                continue

            # 2. Despill en bordes: si el verde sobrepasa excesivamente a R y B en píxeles semitransparentes
            # o en bordes donde se filtró el fondo verde
            max_rb = max(r, b)
            if g > max_rb and (g - max_rb) > 30 and g > 100:
                # Neutralizar derrame de verde sujetándolo al promedio de R y B
                new_g = int((r + b) / 2)
                pixels[x, y] = (r, new_g, b, a)
                despilled_pixels += 1

    img.save(image_path, "PNG")
    print(f"  -> {cleaned_pixels} píxeles verdes transparentados, {despilled_pixels} píxeles despilled.")

def main():
    target_dir = os.path.join("Assets", "Sprites", "Environment")
    # Hojas prioritarias que tienen restos de verde
    targets = [
        "Puestos_Nivl1.png",
        "Contenedores_Sacos_Nvl1.png",
        "Botellas_charcolodo_Nvl1.png",
        "Salidas.png",
        "Puertas_nvl2.png",
        "accesorios_nvl2.png"
    ]

    for filename in targets:
        full_path = os.path.join(target_dir, filename)
        if os.path.exists(full_path):
            clean_image(full_path)
        else:
            print(f"Archivo no encontrado: {full_path}")

if __name__ == "__main__":
    main()
