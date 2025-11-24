package com.example.demo.service;

import com.example.demo.model.Product;
import org.springframework.stereotype.Service;
import jakarta.annotation.PostConstruct;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicLong;

@Service
public class ProductService {

    private final List<Product> products = new ArrayList<>();
    private final AtomicLong seq = new AtomicLong(1);

    @PostConstruct
    public void init() {
        for (int i = 1; i <= 200; i++) {
            products.add(new Product(seq.getAndIncrement(),
                    "Product " + i,
                    (i % 2 == 0) ? "Category A" : "Category B",
                    Math.round((10 + Math.random() * 90) * 100.0) / 100.0));
        }
    }

    public List<Product> getAll() {
        
        int[] delays = {100, 200, 300, 400};
        int randomDelay = delays[(int) (Math.random() * delays.length)];
        try { Thread.sleep(randomDelay); } catch (InterruptedException ignored) {}
        return products;
    }

    public List<Product> getAllDelayed() {
        
        int[] delays = {5000, 10000, 15000};
        int randomDelay = delays[(int) (Math.random() * delays.length)];
        try { Thread.sleep(randomDelay); } catch (InterruptedException ignored) {}
        return products;
    }

    public Product getById(long id) {
        return products.stream().filter(p -> p.getId() == id).findFirst().orElse(null);
    }
}
