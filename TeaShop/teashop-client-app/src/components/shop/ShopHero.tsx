"use client";

import {
    Carousel,
    CarouselContent,
    CarouselItem,
    CarouselNext,
    CarouselPrevious,
} from "@/components/ui/carousel";
import { Button } from "@/components/ui/button";

const slides = [
    {
        subtitle: "VELAS E SABONETES ARTESANAIS ORGÂNICOS",
        heading: "DO JEITO QUE A NATUREZA DESEJA",
        cta: "Comprar agora",
        href: "/shop",
    },
    {
        subtitle: "COLEÇÃO PRIMAVERA",
        heading: "AROMAS QUE RENOVAM A ALMA",
        cta: "Comprar agora",
        href: "/shop",
    },
];

const ShopHero = () => {
    return (
        <section>
            {/* Announcement banner */}
            <div className="bg-[#5c6040] py-2 text-center">
                <p className="text-[10px] font-semibold tracking-[0.2em] text-white">
                    FRETE GRÁTIS EM PEDIDOS ACIMA DE R$250
                </p>
            </div>

            {/* Hero carousel */}
            <Carousel className="w-full" opts={{ loop: true }}>
                <CarouselContent>
                    {slides.map((slide, index) => (
                        <CarouselItem key={index}>
                            <div className="flex min-h-[520px] items-stretch">

                                {/* Left — image placeholder */}
                                <div className="w-1/2 bg-[#e8e4dc]" />

                                {/* Right — text content */}
                                <div className="flex w-1/2 flex-col items-start justify-center gap-6 bg-[#f5f2ec] px-16 py-16">
                                    <p className="text-[11px] font-semibold tracking-[0.2em] text-[#8a7a4a]">
                                        {slide.subtitle}
                                    </p>
                                    <h1 className="font-serif text-5xl font-normal leading-tight text-[#1a1a1a]">
                                        {slide.heading}
                                    </h1>
                                    <Button
                                        asChild
                                        className="mt-2 rounded-none bg-[#5c6040] px-8 py-3 text-[11px] font-semibold tracking-widest text-white hover:bg-[#4a4d32]"
                                    >
                                        <a href={slide.href}>{slide.cta}</a>
                                    </Button>
                                </div>

                            </div>
                        </CarouselItem>
                    ))}
                </CarouselContent>

                <CarouselPrevious className="left-4 border-none bg-white/70 shadow-sm hover:bg-white" />
                <CarouselNext className="right-4 border-none bg-white/70 shadow-sm hover:bg-white" />
            </Carousel>
        </section>
    );
};

export default ShopHero;
